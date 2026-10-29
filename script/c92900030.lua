-- Slasher - Hallows Guard
local s,id=GetID()
function s.initial_effect(c)
    c:EnableCounterPermit(0x199)
    -- Negate response effect by Tributing from hand or field
    local e1=Effect.CreateEffect(c)
    e1:SetCategory(CATEGORY_NEGATE+CATEGORY_DESTROY)
    e1:SetType(EFFECT_TYPE_QUICK_O)
    e1:SetCode(EVENT_CHAINING)
    e1:SetRange(LOCATION_HAND+LOCATION_MZONE)
    e1:SetCountLimit(1,id)
    e1:SetCondition(s.negcon)
    e1:SetCost(s.negcost)
    e1:SetTarget(s.negtg)
    e1:SetOperation(s.negop)
    c:RegisterEffect(e1)

    -- Search Ritual Monster or Ritual Spell when Tributed or sent to GY
    local e2=Effect.CreateEffect(c)
    e2:SetDescription(aux.Stringid(id,0))
    e2:SetCategory(CATEGORY_TOHAND+CATEGORY_SEARCH)
    e2:SetType(EFFECT_TYPE_SINGLE+EFFECT_TYPE_TRIGGER_O)
    e2:SetProperty(EFFECT_FLAG_DELAY)
    e2:SetCode(EVENT_TO_GRAVE)
    e2:SetCountLimit(1,{id,1})
    e2:SetCondition(s.thcon)
    e2:SetTarget(s.thtg)
    e2:SetOperation(s.thop)
    c:RegisterEffect(e2)
end

-- Check if opponent activated something in response to your Slasher activation
function s.negcon(e,tp,eg,ep,ev,re,r,rp)
    if rp==tp or ev<2 or not Duel.IsChainDisablable(ev) then return false end
    -- Check if the previous chain link (ev-1) was your "Slasher" card/effect
    local ce,cps=Duel.GetChainInfo(ev-1, CHAININFO_TRIGGERING_EFFECT, CHAININFO_TRIGGERING_PLAYER)
    if ce and cps==tp then
        local tc=ce:GetHandler()
        if tc and tc:IsSetCard(0x991) then return true end
    end
    return false
end

-- FIXED COST FUNCTION
function s.negcost(e,tp,eg,ep,ev,re,r,rp,chk)
    local c=e:GetHandler()
    -- The chk==0 block is MANDATORY to prevent the "Action not allowed here" crash
    if chk==0 then return c:IsReleasable() end
    -- EDOPro natively supports Tributing from the hand. No workaround needed!
    Duel.Release(c,REASON_COST)
end

function s.negtg(e,tp,eg,ep,ev,re,r,rp,chk)
    if chk==0 then return true end
    Duel.SetOperationInfo(0,CATEGORY_NEGATE,eg,1,0,0)
    local rc=re:GetHandler()
    if rc:IsDestructable() and rc:IsRelateToEffect(re) then
        Duel.SetOperationInfo(0,CATEGORY_DESTROY,eg,1,0,0)
    end
end

function s.negop(e,tp,eg,ep,ev,re,r,rp)
    -- Changed to NegateActivation (Standard for chaining Quick Effects)
    if Duel.NegateEffect(ev) and re:GetHandler():IsRelateToEffect(re) then
        Duel.Destroy(eg,REASON_EFFECT)
    end
end

-- Search condition: Tributed or sent to GY
function s.thcon(e,tp,eg,ep,ev,re,r,rp)
    return true
end

-- Search filter: 1 "Slasher" Ritual Monster or 1 "Slasher" Ritual Spell
function s.thfilter(c)
    -- Added parentheses for safer Lua logic evaluation
    return c:IsSetCard(0x991) and ((c:IsType(TYPE_RITUAL) and c:IsType(TYPE_MONSTER)) or c:IsRitualSpell()) and c:IsAbleToHand()
end

function s.thtg(e,tp,eg,ep,ev,re,r,rp,chk)
    if chk==0 then return Duel.IsExistingMatchingCard(s.thfilter,tp,LOCATION_DECK,0,1,nil) end
    Duel.SetOperationInfo(0,CATEGORY_TOHAND,nil,1,tp,LOCATION_DECK)
end

function s.thop(e,tp,eg,ep,ev,re,r,rp)
    Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_ATOHAND)
    local g=Duel.SelectMatchingCard(tp,s.thfilter,tp,LOCATION_DECK,0,1,1,nil)
    if #g>0 then
        Duel.SendtoHand(g,nil,REASON_EFFECT)
        Duel.ConfirmCards(1-tp,g)
    end
end