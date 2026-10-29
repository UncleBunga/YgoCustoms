-- Slasher - The Miner
local s,id=GetID()
function s.initial_effect(c)
    c:EnableCounterPermit(0x199)
    -- Excavate top 3 cards, add 1, and optional Special Summon
    local e1=Effect.CreateEffect(c)
    e1:SetDescription(aux.Stringid(id,0))
    e1:SetCategory(CATEGORY_TOHAND+CATEGORY_SEARCH+CATEGORY_TOGRAVE+CATEGORY_SPECIAL_SUMMON)
    e1:SetType(EFFECT_TYPE_IGNITION)
    e1:SetRange(LOCATION_MZONE)
    e1:SetCountLimit(1)
    e1:SetTarget(s.thtg)
    e1:SetOperation(s.thop)
    c:RegisterEffect(e1)

    -- Float when sent from field to GY
    local e2=Effect.CreateEffect(c)
    e2:SetDescription(aux.Stringid(id,1))
    e2:SetCategory(CATEGORY_TOHAND)
    e2:SetType(EFFECT_TYPE_SINGLE+EFFECT_TYPE_TRIGGER_O)
    e2:SetProperty(EFFECT_FLAG_DELAY+EFFECT_FLAG_CARD_TARGET)
    e2:SetCode(EVENT_TO_GRAVE)
    e2:SetCondition(s.thcon2)
    e2:SetTarget(s.thtg2)
    e2:SetOperation(s.thop2)
    c:RegisterEffect(e2)
end

-- Excavation targets
function s.thfilter(c)
    return c:IsSetCard(0x991) and c:IsType(TYPE_MONSTER) and c:IsAbleToHand()
end

function s.thtg(e,tp,eg,ep,ev,re,r,rp,chk)
    if chk==0 then return Duel.GetFieldGroupCount(tp,LOCATION_DECK,0)>=3 end
    Duel.SetOperationInfo(0,CATEGORY_TOHAND,nil,1,tp,LOCATION_DECK)
    Duel.SetOperationInfo(0,CATEGORY_TOGRAVE,nil,2,tp,LOCATION_DECK)
end

function s.thop(e,tp,eg,ep,ev,re,r,rp)
    if Duel.GetFieldGroupCount(tp,LOCATION_DECK,0)<3 then return end
    Duel.ConfirmDecktop(tp,3)
    local g=Duel.GetDecktopGroup(tp,3)
    local sc=nil
    if #g>0 then
        Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_ATOHAND)
        local sg=g:IsExists(s.thfilter,1,nil) and g:FilterSelect(tp,s.thfilter,1,1,nil) or Group.CreateGroup()
        if #sg>0 then
            sc=sg:GetFirst()
            Duel.SendtoHand(sg,nil,REASON_EFFECT)
            Duel.ConfirmCards(1-tp,sg)
            g:Sub(sg)
        end
        -- Send remaining cards to GY
        Duel.SendtoGrave(g,REASON_EFFECT+REASON_REVEAL)

        -- Optional: Remove 1 Kill Counter (0x199) to Special Summon the added monster
        if sc and sc:IsLocation(LOCATION_HAND)
            and Duel.IsCanRemoveCounter(tp,1,0,0x199,1,REASON_EFFECT)
            and Duel.GetLocationCount(tp,LOCATION_MZONE)>0
            and sc:IsCanBeSpecialSummoned(e,0,tp,false,false)
            and Duel.SelectYesNo(tp,aux.Stringid(id,2)) then

            Duel.BreakEffect()
            if Duel.RemoveCounter(tp,1,0,0x199,1,REASON_EFFECT) then
                Duel.SpecialSummon(sc,0,tp,tp,false,false,POS_FACEUP)
            end
        end
    end
end

-- Float Condition: Sent from field to GY
function s.thcon2(e,tp,eg,ep,ev,re,r,rp)
    local c=e:GetHandler()
    return c:IsPreviousLocation(LOCATION_ONFIELD)
end

function s.thfilter2(c)
    return c:IsSetCard(0x991) and c:IsType(TYPE_MONSTER) and not c:IsCode(id) and c:IsAbleToHand()
end

function s.thtg2(e,tp,eg,ep,ev,re,r,rp,chk,chkc)
    if chkc then return chkc:IsLocation(LOCATION_GRAVE) and chkc:IsControler(tp) and s.thfilter2(chkc) end
    if chk==0 then return Duel.IsExistingTarget(s.thfilter2,tp,LOCATION_GRAVE,0,1,nil) end
    Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_ATOHAND)
    local g=Duel.SelectTarget(tp,s.thfilter2,tp,LOCATION_GRAVE,0,1,1,nil)
    Duel.SetOperationInfo(0,CATEGORY_TOHAND,g,1,0,0)
end

function s.thop2(e,tp,eg,ep,ev,re,r,rp)
    local tc=Duel.GetFirstTarget()
    if tc and tc:IsRelateToEffect(e) then
        Duel.SendtoHand(tc,nil,REASON_EFFECT)
        Duel.ConfirmCards(1-tp,tc)
    end
end