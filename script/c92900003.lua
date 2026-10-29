-- Slasher - Possessed Puppet
local s,id=GetID()

function s.initial_effect(c)
    c:EnableCounterPermit(0x199)
    -- Must be properly Special Summoned
    c:EnableReviveLimit()

    -- Can attack directly
    local e1a=Effect.CreateEffect(c)
    e1a:SetType(EFFECT_TYPE_SINGLE)
    e1a:SetCode(EFFECT_DIRECT_ATTACK)
    c:RegisterEffect(e1a)

    -- When Ritual Summoned: Send 1 "Slasher" monster from Deck to GY
    local e1=Effect.CreateEffect(c)
    e1:SetDescription(aux.Stringid(id,0))
    e1:SetCategory(CATEGORY_TOGRAVE)
    e1:SetType(EFFECT_TYPE_SINGLE+EFFECT_TYPE_TRIGGER_O)
    e1:SetProperty(EFFECT_FLAG_DELAY)
    e1:SetCode(EVENT_SPSUMMON_SUCCESS)
    e1:SetCondition(s.thcon)
    e1:SetCountLimit(1,id)
    e1:SetTarget(s.thtg)
    e1:SetOperation(s.thop)
    c:RegisterEffect(e1)

    -- Ignition (Hand): Reveal this card + 1 other "Slasher" monster in hand; shuffle 1 into Deck, then draw 1
    local e2=Effect.CreateEffect(c)
    e2:SetDescription(aux.Stringid(id,1))
    e2:SetCategory(CATEGORY_TODECK+CATEGORY_DRAW)
    e2:SetType(EFFECT_TYPE_IGNITION)
    e2:SetRange(LOCATION_HAND)
    e2:SetCountLimit(1,{id,1})
    e2:SetCost(s.revcost)
    e2:SetTarget(s.revtg)
    e2:SetOperation(s.revop)
    c:RegisterEffect(e2)

    -- Ignition (Field): Remove 1 Kill Counter from anywhere on the field, then target 1 face-up monster opponent controls; its ATK/DEF become 0 until end of turn
    local e3=Effect.CreateEffect(c)
    e3:SetDescription(aux.Stringid(id,2))
    e3:SetCategory(CATEGORY_ATKCHANGE+CATEGORY_DEFCHANGE)
    e3:SetType(EFFECT_TYPE_IGNITION)
    e3:SetRange(LOCATION_MZONE)
    e3:SetProperty(EFFECT_FLAG_CARD_TARGET)
    e3:SetCountLimit(1,{id,2})
    e3:SetCost(s.wipecost)
    e3:SetTarget(s.wipetg)
    e3:SetOperation(s.wipeop)
    c:RegisterEffect(e3)
end

-- Condition: Check if the summon type was a Ritual Summon
function s.thcon(e,tp,eg,ep,ev,re,r,rp)
    return e:GetHandler():IsSummonType(SUMMON_TYPE_RITUAL)
end

-- Filter: "Slasher" monster from Deck to GY
function s.thfilter(c)
    return c:IsSetCard(0x991) and c:IsType(TYPE_MONSTER) and c:IsAbleToGrave()
end

function s.thtg(e,tp,eg,ep,ev,re,r,rp,chk)
    if chk==0 then return Duel.IsExistingMatchingCard(s.thfilter,tp,LOCATION_DECK,0,1,nil) end
    Duel.SetOperationInfo(0,CATEGORY_TOGRAVE,nil,1,tp,LOCATION_DECK)
end

function s.thop(e,tp,eg,ep,ev,re,r,rp)
    Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_TOGRAVE)
    local g=Duel.SelectMatchingCard(tp,s.thfilter,tp,LOCATION_DECK,0,1,1,nil)
    if #g>0 then
        Duel.SendtoGrave(g,REASON_EFFECT)
    end
end

-- Cost: reveal this card and 1 other "Slasher" monster in hand
function s.cfilter(c)
    return c:IsSetCard(0x991) and c:IsMonster() and not c:IsPublic()
end

function s.revcost(e,tp,eg,ep,ev,re,r,rp,chk)
    local c=e:GetHandler()
    if chk==0 then return not c:IsPublic() and Duel.IsExistingMatchingCard(s.cfilter,tp,LOCATION_HAND,0,1,c) end
    Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_CONFIRM)
    local g=Duel.SelectMatchingCard(tp,s.cfilter,tp,LOCATION_HAND,0,1,1,c)
    g:AddCard(c)
    Duel.ConfirmCards(1-tp,g)
    Duel.ShuffleHand(tp)
    g:KeepAlive()
    e:SetLabelObject(g)
end

function s.revtg(e,tp,eg,ep,ev,re,r,rp,chk)
    if chk==0 then return Duel.IsPlayerCanDraw(tp,1) end
    Duel.SetOperationInfo(0,CATEGORY_TODECK,nil,1,tp,LOCATION_HAND)
    Duel.SetOperationInfo(0,CATEGORY_DRAW,nil,0,tp,1)
end

-- Shuffle 1 of the revealed cards into the Deck, and if you do, draw 1
function s.revop(e,tp,eg,ep,ev,re,r,rp)
    local g=e:GetLabelObject()
    if not g then return end
    local rg=g:Filter(Card.IsLocation,nil,LOCATION_HAND)
    g:DeleteGroup()
    e:SetLabelObject(nil)
    if #rg==0 then return end
    Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_TODECK)
    local sg=rg:FilterSelect(tp,Card.IsAbleToDeck,1,1,nil)
    if #sg>0 and Duel.SendtoDeck(sg,nil,SEQ_DECKSHUFFLE,REASON_EFFECT)>0 and sg:GetFirst():IsLocation(LOCATION_DECK) then
        Duel.Draw(tp,1,REASON_EFFECT)
    end
end

-- Kill Counter Cost
function s.wipecost(e,tp,eg,ep,ev,re,r,rp,chk)
    if chk==0 then return Duel.IsCanRemoveCounter(tp,1,1,0x199,1,REASON_COST) end
    Duel.RemoveCounter(tp,1,1,0x199,1,REASON_COST)
end

function s.wipetg(e,tp,eg,ep,ev,re,r,rp,chk,chkc)
    if chkc then return chkc:IsLocation(LOCATION_MZONE) and chkc:IsControler(1-tp) and chkc:IsFaceup() end
    if chk==0 then return Duel.IsExistingTarget(Card.IsFaceup,tp,0,LOCATION_MZONE,1,nil) end
    Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_TARGET)
    local g=Duel.SelectTarget(tp,Card.IsFaceup,tp,0,LOCATION_MZONE,1,1,nil)
    Duel.SetOperationInfo(0,CATEGORY_ATKCHANGE,g,1,0,0)
    Duel.SetOperationInfo(0,CATEGORY_DEFCHANGE,g,1,0,0)
end

function s.wipeop(e,tp,eg,ep,ev,re,r,rp)
    local tc=Duel.GetFirstTarget()
    if tc and tc:IsRelateToEffect(e) and tc:IsFaceup() then
        local c=e:GetHandler()
        -- Set ATK to 0
        local e1=Effect.CreateEffect(c)
        e1:SetType(EFFECT_TYPE_SINGLE)
        e1:SetCode(EFFECT_SET_ATTACK_FINAL)
        e1:SetValue(0)
        e1:SetReset(RESET_EVENT+RESETS_STANDARD+RESET_PHASE+PHASE_END)
        tc:RegisterEffect(e1)
        -- Set DEF to 0
        local e2=e1:Clone()
        e2:SetCode(EFFECT_SET_DEFENSE_FINAL)
        tc:RegisterEffect(e2)
    end
end